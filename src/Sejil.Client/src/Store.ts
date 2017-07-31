import { runInAction, observable, action } from 'mobx';
import { HttpClient } from './HttpClient';
import ILogEntry from './interfaces/ILogEntry';
import ILogQuery from './interfaces/ILogQuery';

export default class Store {
    @observable logEntries: ILogEntry[] = [];
    @observable queries: ILogQuery[] = [];
    @observable queryText = '';
    private http = new HttpClient();
    private page = 1;
    private startingTimestamp: string | undefined = undefined;
    private rootUrl = window.location.pathname;

    constructor() {
        this.loadEvents();
        this.loadQueries();
    }

    @action public async filterEvents() {
        this.page = 1;
        this.startingTimestamp = undefined;
        await this.loadEvents(false);
    }

    @action public async loadEvents(concat = true) {
        const url = this.startingTimestamp
            ? `${this.rootUrl}/events?page=${this.page}&startingTs=${this.startingTimestamp}`
            : `${this.rootUrl}/events?page=${this.page}`;

        const json = await this.http.post(url, this.queryText);
        const events = JSON.parse(json) as ILogEntry[];

        if (!this.startingTimestamp && events.length) {
            this.startingTimestamp = events[0].timestamp;
        }

        runInAction('load entries', () => {
            if (concat) {
                this.logEntries = this.logEntries.concat(events);
                this.page++;
            } else {
                this.logEntries = events;
            }
        });
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

        if (queries.length) {
            runInAction('load queries',
                () => { this.queries = queries; });
        }
    }
}
