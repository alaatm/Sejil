// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import { ILogEntry, ILogQuery } from './interfaces';
import { action, observable, runInAction } from 'mobx';

import { formatServerDate } from './misc/formatDate';

export default class Store {
    @observable logEntries: ILogEntry[] = [];
    @observable queries: ILogQuery[] = [];
    @observable queryText = '';
    @observable minLogLevel = '';
    dateFilter: string | Date[] | null = null;
    levelFilter: string | null = null;
    exceptionsOnly = false;
    private page = 1;
    private startingTimestamp: string | undefined = undefined;
    private rootUrl = window.location.pathname;

    @action public async reset() {
        this.page = 1;
        this.startingTimestamp = undefined;
        this.logEntries = [];
        await this.loadEvents();
    }

    @action public async loadEvents() {
        const url = this.startingTimestamp
            ? `${this.rootUrl}/events?page=${this.page}&startingTs=${encodeURIComponent(this.startingTimestamp)}`
            : `${this.rootUrl}/events?page=${this.page}`;

        const response = await fetch(url, {
            method: 'post',
            body: JSON.stringify({
                queryText: this.queryText,
                dateFilter: this.dateFilter instanceof Array
                    ? null
                    : this.dateFilter,
                dateRangeFilter: this.dateFilter instanceof Array
                    ? this.dateFilter.map(d => formatServerDate(d))
                    : null,
                levelFilter: this.levelFilter,
                exceptionsOnly: this.exceptionsOnly,
            })
        });
        const events = await response.json() as ILogEntry[];

        if (events.length) {
            if (!this.startingTimestamp) {
                this.startingTimestamp = events[0].timestamp;
            }

            runInAction('load entries', () => this.logEntries = this.logEntries.concat(events));
            this.page++;
        }
    }

    @action public async saveQuery(name: string, query: string) {
        await fetch(`${this.rootUrl}/log-query`, {
            method: 'post',
            body: JSON.stringify({ name, query })
        });
        runInAction(
            'save query',
            () => {
                this.queries.push({
                    name,
                    query,
                });
            });
    }

    @action public async loadQueries() {
        const response = await fetch(`${this.rootUrl}/log-queries`);
        const queries = await response.json() as ILogQuery[];

        runInAction('load queries', () => this.queries = queries);
    }

    @action public async deleteQuery(q: ILogQuery) {
        await fetch(`${this.rootUrl}/del-query`, {
            method: 'post',
            body: q.name
        });
        runInAction('delete query', () => {
            const index = this.queries.findIndex(p => p.name === q.name);
            if (index >= 0) {
                this.queries.splice(index, 1);
            }
        });
    }

    @action public async loadMinLogLevel() {
        const response = await fetch(`${this.rootUrl}/min-log-level`);
        const responseJson = await response.json() as { minimumLogLevel: string };

        runInAction('load min log level', () => this.minLogLevel = responseJson.minimumLogLevel);
    }

    @action public async setMinLogLevel(level: string) {
        await fetch(`${this.rootUrl}/min-log-level`, {
            method: 'post',
            body: level
        });
        runInAction('set min log level', () => this.minLogLevel = level);
    }
}
