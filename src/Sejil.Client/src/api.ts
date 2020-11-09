import { LogEntry, LogQuery, LogQueryFilter } from './Models';
import dayjs from 'dayjs';

const api = {
    rootUrl: window.location.pathname,

    getLogEvents: async function (page: number, filter: LogQueryFilter, startingTimestamp?: string) {
        const url = startingTimestamp
            ? `${this.rootUrl}/events?page=${page}&startingTs=${encodeURIComponent(startingTimestamp)}`
            : `${this.rootUrl}/events?page=${page}`;

        const response = await fetch(url, {
            method: 'post',
            body: JSON.stringify({ ...filter, dateRangeFilter: this._serverDate(filter.dateRangeFilter) }),
        });

        if (response.status === 400) {
            const { error } = await response.json() as { error: string };
            return { error, events: [] as LogEntry[] };
        }

        return { error: undefined, events: await response.json() as LogEntry[] };
    },

    getSavedQueries: async function () {
        const response = await fetch(`${this.rootUrl}/log-queries`);
        return await response.json() as LogQuery[];
    },

    saveQuery: async function (query: LogQuery) {
        await fetch(`${this.rootUrl}/log-query`, {
            method: 'post',
            body: JSON.stringify(query),
        });
    },

    deleteQuery: async function (name: string) {
        await fetch(`${this.rootUrl}/del-query`, {
            method: 'post',
            body: name,
        });
    },

    getLoggedInUser: async function () {
        const response = await fetch(`${this.rootUrl}/user-name`);
        const json = await response.json() as { userName: string };
        return json.userName;
    },

    getTitle: async function () {
        const response = await fetch(`${this.rootUrl}/title`);
        const json = await response.json() as { title: string };
        return json.title;
    },

    getMinLogLevel: async function () {
        const response = await fetch(`${this.rootUrl}/min-log-level`);
        const json = await response.json() as { minimumLogLevel: string };
        return json.minimumLogLevel;
    },

    setMinLogLevel: async function (level: string) {
        await fetch(`${this.rootUrl}/min-log-level`, {
            method: 'post',
            body: level,
        });
    },

    _serverDate: function (dateRangeFilter?: [dayjs.Dayjs, dayjs.Dayjs]) {
        if (dateRangeFilter) {
            return [dateRangeFilter[0].format('YYYY-MM-DD'), dateRangeFilter[1].format('YYYY-MM-DD')];
        }

        return undefined;
    }
};

export default api;