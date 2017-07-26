import { runInAction, observable, action } from 'mobx';
import { HttpClient } from './HttpClient';
import IEventEntry from './interfaces/IEventEntry';
import IFilter from './interfaces/IFilter';

export default class Store {
    @observable logEntries: IEventEntry[] = [];
    @observable filters: IFilter[] = [];
    @observable filterText = '';
    private http = new HttpClient();
    private page = 1;
    private startingTimestamp: string | undefined = undefined;
    private rootUrl = window.location.pathname;

    constructor() {
        this.loadEvents();
        this.loadFilters();
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

        const defaultFilter = `RequestPath not like '%${this.rootUrl}%' && Path not like '%${this.rootUrl}%'`;
        const filter = this.filterText
            ? `${defaultFilter} && (${this.filterText})`
            : defaultFilter;

        const json = await this.http.post(url, filter);
        const events = JSON.parse(json) as IEventEntry[];

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

    @action public async saveFilter(name: string, filter: string) {
        await this.http.post(`${this.rootUrl}/filter?name=${name}`, filter);
        runInAction('save filter',
            () => {
                this.filters.push({
                    name,
                    filter,
                });
            });
    }

    @action public async loadFilters() {
        const json = await this.http.get(`${this.rootUrl}/filters`);
        const filters = JSON.parse(json) as IFilter[];

        if (filters.length) {
            runInAction('load filters',
                () => { this.filters = filters; });
        }
    }
}
