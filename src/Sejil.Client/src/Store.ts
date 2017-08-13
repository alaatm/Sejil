// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import { runInAction, observable, action } from 'mobx';
import { IHttpClient, HttpClient } from './HttpClient';
import ILogEntry from './interfaces/ILogEntry';
import ILogQuery from './interfaces/ILogQuery';
import { formatServerDate } from './formatDate';

export default class Store {
    @observable logEntries: ILogEntry[] = [];
    @observable queries: ILogQuery[] = [];
    @observable queryText = '';
    dateFilter: string | Date[] | null = null;
    levelFilter: string | null = null;
    exceptionsOnly = false;
    private http: IHttpClient;
    private page = 1;
    private startingTimestamp: string | undefined = undefined;
    private rootUrl = window.location.pathname;

    constructor(http: IHttpClient = new HttpClient()) {
        this.http = http;
    }

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

        const json = await this.http.post(url, JSON.stringify({
            queryText: this.queryText,
            dateFilter: this.dateFilter instanceof Array ? null : this.dateFilter,
            dateRangeFilter: this.dateFilter instanceof Array ? this.dateFilter.map(d => formatServerDate(d)) : null,
            levelFilter: this.levelFilter,
            exceptionsOnly: this.exceptionsOnly,
        }));
        const events = JSON.parse(json) as ILogEntry[];

        if (events.length) {
            if (!this.startingTimestamp) {
                this.startingTimestamp = events[0].timestamp;
            }

            runInAction('load entries', () => this.logEntries = this.logEntries.concat(events));
            this.page++;
        }
    }

    @action public async saveQuery(name: string, query: string) {
        await this.http.post(`${this.rootUrl}/log-query`, JSON.stringify({ name, query }));
        runInAction('save query',
            () => {
                this.queries.push({
                    name,
                    query,
                });
            });
    }

    @action public async loadQueries() {
        const json = await this.http.get(`${this.rootUrl}/log-queries`);
        const queries = JSON.parse(json) as ILogQuery[];

        runInAction('load queries', () => this.queries = queries);
    }

    @action public async deleteQuery(q: ILogQuery) {
        await this.http.post(`${this.rootUrl}/del-query`, q.name);
        runInAction('delete query', () => {
            const index = this.queries.findIndex(p => p.name === q.name);
            if (index >= 0) {
                this.queries.splice(index, 1);
            }
        });
    }
}
