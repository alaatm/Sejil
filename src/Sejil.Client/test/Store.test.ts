// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import { toJS } from 'mobx';
import Store from '../src/Store';
import { IHttpClient, HttpClient } from '../src/HttpClient';
import { createTestLogEntries, createTestLogQueries, createTestQuery } from './testHelpers';

const rootUrl = window.location.pathname;

describe('Store', () => {
    it('loadEvents() should load events', async () => {
        // Arrange
        const testEvents = createTestLogEntries(0, 5);

        const Mock = jest.fn<IHttpClient>(() => ({
            get: jest.fn(),
            post: jest.fn(() => testEvents.eventsJson),
        }));

        const httpClientMoq = new Mock();
        const store = new Store(httpClientMoq);

        // Act
        await store.loadEvents();

        // Assert
        expect(httpClientMoq.post).toHaveBeenCalled();
        expect(httpClientMoq.post).toHaveBeenCalledWith(`${rootUrl}/events?page=1`, '{"queryText":"","dateFilter":null,"dateRangeFilter":null}');
        expect(toJS(store.logEntries)).toHaveLength(testEvents.events.length);
        expect(toJS(store.logEntries)).toMatchObject(testEvents.events);
    });

    it('loadEvents() should load next set of events on next call and append to event list', async () => {
        // Arrange
        const testEvents_page1 = createTestLogEntries(0, 10);
        const testEvents_page2 = createTestLogEntries(10, 20);
        const testEvents_page3 = createTestLogEntries(20, 30);

        const Mock = jest.fn<HttpClient>(() => ({
            get: jest.fn(),
            post: jest.fn(() => { throw new Error(); })
                .mockImplementationOnce(() => testEvents_page1.eventsJson)  // Page 1 on first call
                .mockImplementationOnce(() => testEvents_page2.eventsJson)  // Page 2 on second call
                .mockImplementationOnce(() => testEvents_page3.eventsJson), // Page 3 on third call
        }));

        const httpClientMoq = new Mock();
        const store = new Store(httpClientMoq);

        const expectedPageNumber = 3;
        const expectedTimestamp = testEvents_page1.events[0].timestamp;

        // Act
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();

        // Assert
        expect(httpClientMoq.post).toHaveBeenCalledTimes(3);
        expect(httpClientMoq.post).toHaveBeenLastCalledWith(`${rootUrl}/events?page=${expectedPageNumber}&startingTs=${expectedTimestamp}`, '{"queryText":"","dateFilter":null,"dateRangeFilter":null}');
        expect(toJS(store.logEntries)).toHaveLength(testEvents_page1.events.length + testEvents_page2.events.length + testEvents_page3.events.length);
        expect(toJS(store.logEntries)).toMatchObject(testEvents_page1.events.concat(testEvents_page2.events.concat(testEvents_page3.events)));
    });

    it('loadEvents() should not set startingTimestamp when no results are returned', async () => {

        const Mock = jest.fn<HttpClient>(() => ({
            get: jest.fn(),
            post: jest.fn(() => '[]') // Empty array on all calls
        }));

        const httpClientMoq = new Mock();
        const store = new Store(httpClientMoq);

        // Act
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();

        // Assert
        expect(httpClientMoq.post).toHaveBeenLastCalledWith(`${rootUrl}/events?page=1`, '{"queryText":"","dateFilter":null,"dateRangeFilter":null}');
    });

    it('loadEvents() should not advance page number when no results are returned', async () => {
        // Arrange
        const testEvents_page1 = createTestLogEntries(0, 10);
        const testEvents_page2 = createTestLogEntries(10, 15);

        const Mock = jest.fn<HttpClient>(() => ({
            get: jest.fn(),
            post: jest.fn(() => '[]') // Empty array on all calls after the second call
                .mockImplementationOnce(() => testEvents_page1.eventsJson)  // Page 1 on first call
                .mockImplementationOnce(() => testEvents_page2.eventsJson)  // Page 2 on second call
        }));

        const httpClientMoq = new Mock();
        const store = new Store(httpClientMoq);

        const expectedPageNumber = 3;

        // Act
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();

        // Assert
        expect(httpClientMoq.post).toHaveBeenLastCalledWith(`${rootUrl}/events?page=${expectedPageNumber}&startingTs=${testEvents_page1.events[0].timestamp}`, '{"queryText":"","dateFilter":null,"dateRangeFilter":null}');
    });

    it('loadEvents() should add queryText and datefilters as content when set', async () => {
        // Arrange
        const Mock = jest.fn<HttpClient>(() => ({
            get: jest.fn(),
            post: jest.fn(() => '[]')
        }));

        const httpClientMoq = new Mock();
        const store = new Store(httpClientMoq);
        store.queryText = 'prop=val';
        store.dateFilter = '5m';

        // Act
        await store.loadEvents();

        // Assert
        expect(httpClientMoq.post).toHaveBeenLastCalledWith(`${rootUrl}/events?page=1`, '{"queryText":"prop=val","dateFilter":"5m","dateRangeFilter":null}');
    });

    it('loadEvents() should add date range filters as content when set', async () => {
        // Arrange
        const Mock = jest.fn<HttpClient>(() => ({
            get: jest.fn(),
            post: jest.fn(() => '[]')
        }));

        const httpClientMoq = new Mock();
        const store = new Store(httpClientMoq);
        store.queryText = 'prop=val';
        store.dateFilter = [new Date(2017, 8, 1), new Date(2017, 8, 10)];

        // Act
        await store.loadEvents();

        // Assert
        expect(httpClientMoq.post).toHaveBeenLastCalledWith(`${rootUrl}/events?page=1`, 
            '{"queryText":"prop=val","dateFilter":null,"dateRangeFilter":["2017-09-01","2017-09-10"]}');
    });

    it('reset() should load first set of events', async () => {
        // Arrange
        const testEvents_page1 = createTestLogEntries(0, 10);
        const testEvents_page2 = createTestLogEntries(10, 20);

        const Mock = jest.fn<HttpClient>(() => ({
            get: jest.fn(),
            post: jest.fn(() => '[]')
                .mockImplementationOnce(() => testEvents_page1.eventsJson)  // Page 1 on first call
                .mockImplementationOnce(() => testEvents_page2.eventsJson)  // Page 2 on second call
        }));

        const httpClientMoq = new Mock();
        const store = new Store(httpClientMoq);

        // Act
        await store.loadEvents();
        await store.loadEvents();
        await store.reset();

        // Assert
        expect(httpClientMoq.post).toHaveBeenLastCalledWith(`${rootUrl}/events?page=1`, '{"queryText":"","dateFilter":null,"dateRangeFilter":null}');
    });

    it('saveQuery() should save query', async () => {
        // Arrange
        const Mock = jest.fn<HttpClient>(() => ({
            get: jest.fn(),
            post: jest.fn(),
        }));

        const httpClientMoq = new Mock();
        const store = new Store(httpClientMoq);

        const query = createTestQuery();

        // Act
        await store.saveQuery(query.name, query.query);

        // Assert
        expect(httpClientMoq.post).toHaveBeenCalled();
        expect(httpClientMoq.post).toHaveBeenCalledWith(`${rootUrl}/log-query`, JSON.stringify(query));
        expect(store.queries).toHaveLength(1);
        expect(toJS(store.queries)).toMatchObject([query]);
    });

    it('loadQueries() should load saved queries', async () => {
        // Arrange
        const testQueries = createTestLogQueries();

        const Mock = jest.fn<HttpClient>(() => ({
            get: jest.fn(() => testQueries.queriesJson),
            post: jest.fn(),
        }));

        const httpClientMoq = new Mock();
        const store = new Store(httpClientMoq);

        // Act
        await store.loadQueries();

        // Assert
        expect(httpClientMoq.get).toHaveBeenCalled();
        expect(httpClientMoq.get).toHaveBeenCalledWith(`${rootUrl}/log-queries`);
        expect(toJS(store.queries)).toHaveLength(testQueries.queries.length);
        expect(toJS(store.queries)).toMatchObject(testQueries.queries);
    });
});
