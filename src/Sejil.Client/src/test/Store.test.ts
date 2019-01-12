// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import { createTestLogEntries, createTestLogQueries, createTestQuery } from './testHelpers';

import Store from '../Store';
import { toJS } from 'mobx';

const rootUrl = window.location.pathname;

(global as any).fetch = require('jest-fetch-mock');

describe('Store', () => {
    it('loadEvents() should load events', async () => {
        // Arrange
        const testEvents = createTestLogEntries(0, 5);
        fetch.mockResponse(testEvents.eventsJson);

        const store = new Store();

        // Act
        await store.loadEvents();

        // Assert
        expect(fetch).toHaveBeenCalled();
        expect(fetch).toHaveBeenCalledWith(
            `${rootUrl}/events?page=1`,
            buildRequestObject(
                'post',
                '{"queryText":"","dateFilter":null,"dateRangeFilter":null,"levelFilter":null,"exceptionsOnly":false}'));
        expect(toJS(store.logEntries)).toHaveLength(testEvents.events.length);
        expect(toJS(store.logEntries)).toMatchObject(testEvents.events);

        fetch.resetMocks();
    });

    it('loadEvents() should load next set of events on next call and append to event list', async () => {
        // Arrange
        const testEvents_page1 = createTestLogEntries(0, 10);
        const testEvents_page2 = createTestLogEntries(10, 20);
        const testEvents_page3 = createTestLogEntries(20, 30);

        fetch.mockResponseOnce(testEvents_page1.eventsJson); // Page 1 on first call
        fetch.mockResponseOnce(testEvents_page2.eventsJson); // Page 2 on second call
        fetch.mockResponseOnce(testEvents_page3.eventsJson); // Page 3 on second call

        const store = new Store();

        const expectedPageNumber = 3;
        const expectedTimestamp = encodeURIComponent(testEvents_page1.events[0].timestamp);

        // Act
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();

        // Assert
        expect(fetch).toHaveBeenCalledTimes(3);
        expect(fetch).toHaveBeenLastCalledWith(
            `${rootUrl}/events?page=${expectedPageNumber}&startingTs=${expectedTimestamp}`,
            buildRequestObject(
                'post',
                '{"queryText":"","dateFilter":null,"dateRangeFilter":null,"levelFilter":null,"exceptionsOnly":false}'
            ));
        expect(toJS(store.logEntries)).toHaveLength(
            testEvents_page1.events.length + testEvents_page2.events.length + testEvents_page3.events.length);
        expect(toJS(store.logEntries)).toMatchObject(
            testEvents_page1.events.concat(testEvents_page2.events.concat(testEvents_page3.events)));

        fetch.resetMocks();
    });

    it('loadEvents() should not set startingTimestamp when no results are returned', async () => {
        // Arrange
        fetch.mockResponse('[]'); // Empty array on all calls

        const store = new Store();

        // Act
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();

        // Assert
        expect(fetch).toHaveBeenLastCalledWith(
            `${rootUrl}/events?page=1`,
            buildRequestObject(
                'post',
                '{"queryText":"","dateFilter":null,"dateRangeFilter":null,"levelFilter":null,"exceptionsOnly":false}'));

        fetch.resetMocks();
    });

    it('loadEvents() should not advance page number when no results are returned', async () => {
        // Arrange
        const testEvents_page1 = createTestLogEntries(0, 10);
        const testEvents_page2 = createTestLogEntries(10, 15);

        fetch.mockResponseOnce(testEvents_page1.eventsJson); // Page 1 on first call
        fetch.mockResponseOnce(testEvents_page2.eventsJson); // Page 2 on first call
        fetch.mockResponse('{}'); // No results
        fetch.mockResponse('{}'); // No results
        fetch.mockResponse('{}'); // No results

        const store = new Store();

        const expectedPageNumber = 3;

        // Act
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();
        await store.loadEvents();

        // Assert
        expect(fetch).toHaveBeenLastCalledWith(
            `${rootUrl}/events?page=${expectedPageNumber}&startingTs=${encodeURIComponent(testEvents_page1.events[0].timestamp)}`,
            buildRequestObject(
                'post',
                '{"queryText":"","dateFilter":null,"dateRangeFilter":null,"levelFilter":null,"exceptionsOnly":false}'));

        fetch.resetMocks();
    });

    it('loadEvents() should add queryText as content when set', async () => {
        // Arrange
        fetch.mockResponse('[]');

        const store = new Store();
        store.queryText = 'prop=val';

        // Act
        await store.loadEvents();

        // Assert
        expect(fetch).toHaveBeenLastCalledWith(
            `${rootUrl}/events?page=1`,
            buildRequestObject(
                'post',
                '{"queryText":"prop=val","dateFilter":null,"dateRangeFilter":null,"levelFilter":null,"exceptionsOnly":false}'));

        fetch.resetMocks();
    });

    it('loadEvents() should add date filter as content when set', async () => {
        // Arrange
        fetch.mockResponse('[]');

        const store = new Store();
        store.dateFilter = '5m';

        // Act
        await store.loadEvents();

        // Assert
        expect(fetch).toHaveBeenLastCalledWith(
            `${rootUrl}/events?page=1`,
            buildRequestObject(
                'post',
                '{"queryText":"","dateFilter":"5m","dateRangeFilter":null,"levelFilter":null,"exceptionsOnly":false}'));

        fetch.resetMocks();
    });

    it('loadEvents() should add date range filters as content when set', async () => {
        // Arrange
        fetch.mockResponse('[]');

        const store = new Store();
        store.queryText = 'prop=val';
        store.dateFilter = [new Date(2017, 8, 1), new Date(2017, 8, 10)];

        // Act
        await store.loadEvents();

        // Assert
        expect(fetch).toHaveBeenLastCalledWith(
            `${rootUrl}/events?page=1`,
            buildRequestObject(
                'post',
                '{"queryText":"prop=val","dateFilter":null,"dateRangeFilter":["2017-09-01","2017-09-10"],"levelFilter":null,"exceptionsOnly":false}'));

        fetch.resetMocks();
    });

    it('loadEvents() should add level filter as content when set', async () => {
        // Arrange
        fetch.mockResponse('[]');

        const store = new Store();
        store.levelFilter = 'info';

        // Act
        await store.loadEvents();

        // Assert
        expect(fetch).toHaveBeenLastCalledWith(
            `${rootUrl}/events?page=1`,
            buildRequestObject(
                'post',
                '{"queryText":"","dateFilter":null,"dateRangeFilter":null,"levelFilter":"info","exceptionsOnly":false}'));

        fetch.resetMocks();
    });

    it('loadEvents() should add exception filter as content when set', async () => {
        // Arrange
        fetch.mockResponse('[]');

        const store = new Store();
        store.exceptionsOnly = true;

        // Act
        await store.loadEvents();

        // Assert
        expect(fetch).toHaveBeenLastCalledWith(
            `${rootUrl}/events?page=1`,
            buildRequestObject(
                'post',
                '{"queryText":"","dateFilter":null,"dateRangeFilter":null,"levelFilter":null,"exceptionsOnly":true}'));

        fetch.resetMocks();
    });

    it('reset() should load first set of events', async () => {
        // Arrange
        const testEvents_page1 = createTestLogEntries(0, 10);
        const testEvents_page2 = createTestLogEntries(10, 20);

        fetch.mockResponseOnce(testEvents_page1.eventsJson); // Page 1 on first call
        fetch.mockResponseOnce(testEvents_page2.eventsJson); // Page 2 on first call
        fetch.mockResponseOnce('{}'); // Page 2 on first call

        const store = new Store();

        // Act
        await store.loadEvents();
        await store.loadEvents();
        await store.reset();

        // Assert
        expect(fetch).toHaveBeenLastCalledWith(
            `${rootUrl}/events?page=1`,
            buildRequestObject(
                'post',
                '{"queryText":"","dateFilter":null,"dateRangeFilter":null,"levelFilter":null,"exceptionsOnly":false}'));

        fetch.resetMocks();
    });

    it('saveQuery() should save query', async () => {
        // Arrange
        fetch.mockResponse('');

        const store = new Store();

        const query = createTestQuery();

        // Act
        await store.saveQuery(query.name, query.query);

        // Assert
        expect(fetch).toHaveBeenCalled();
        expect(fetch).toHaveBeenCalledWith(`${rootUrl}/log-query`, buildRequestObject('post', JSON.stringify(query)));
        expect(store.queries).toHaveLength(1);
        expect(toJS(store.queries)).toMatchObject([query]);

        fetch.resetMocks();
    });

    it('loadQueries() should load saved queries', async () => {
        // Arrange
        const testQueries = createTestLogQueries();
        fetch.mockResponse(testQueries.queriesJson);

        const store = new Store();

        // Act
        await store.loadQueries();

        // Assert
        expect(fetch).toHaveBeenCalled();
        expect(fetch).toHaveBeenCalledWith(`${rootUrl}/log-queries`);
        expect(toJS(store.queries)).toHaveLength(testQueries.queries.length);
        expect(toJS(store.queries)).toMatchObject(testQueries.queries);

        fetch.resetMocks();
    });

    it('deleteQuery() should delete query', async () => {
        // Arrange
        fetch.mockResponse('');

        const store = new Store();

        const query = createTestQuery();
        await store.saveQuery(query.name, query.query);

        // Act
        await store.deleteQuery(query);

        // Assert
        expect(fetch).toHaveBeenCalled();
        expect(fetch).toHaveBeenCalledWith(`${rootUrl}/del-query`, buildRequestObject('post', query.name));
        expect(store.queries).toHaveLength(0);
        expect(toJS(store.queries)).toMatchObject([]);

        fetch.resetMocks();
    });

    it('deleteQuery() does nothing when query does not exist', async () => {
        // Arrange
        fetch.mockResponse('');

        const store = new Store();

        const query = createTestQuery();

        // Act
        await store.deleteQuery(query);

        // Assert
        expect(fetch).toHaveBeenCalled();
        expect(fetch).toHaveBeenCalledWith(`${rootUrl}/del-query`, buildRequestObject('post', query.name));
        expect(store.queries).toHaveLength(0);
        expect(toJS(store.queries)).toMatchObject([]);

        fetch.resetMocks();
    });

    it('loadMinLogLevel() should retreive and set minimum log level', async () => {
        // Arrange
        const logLevel = 'Debug';
        const testMinLogLevel = `{ "minimumLogLevel": "${logLevel}" }`;
        fetch.mockResponse(testMinLogLevel);

        const store = new Store();

        // Act
        await store.loadMinLogLevel();

        // Assert
        expect(fetch).toHaveBeenCalled();
        expect(fetch).toHaveBeenCalledWith(`${rootUrl}/min-log-level`);
        expect(store.minLogLevel).toEqual('Debug');

        fetch.resetMocks();
    });

    it('setMinLogLevel() should set minimum log level', async () => {
        // Arrange
        fetch.mockResponse('');
        const store = new Store();

        const logLevel = 'Information';

        // Act
        await store.setMinLogLevel(logLevel);

        // Assert
        expect(fetch).toHaveBeenCalled();
        expect(fetch).toHaveBeenCalledWith(`${rootUrl}/min-log-level`, buildRequestObject('post', logLevel));
        expect(store.minLogLevel).toEqual(logLevel);

        fetch.resetMocks();
    });

    it('loadUserName() should retreive and set user name', async () => {
        // Arrange
        const username = 'john_doe';
        const testUsername = `{ "userName": "${username}" }`;
        fetch.mockResponse(testUsername);

        const store = new Store();

        // Act
        await store.loadUserName();

        // Assert
        expect(fetch).toHaveBeenCalled();
        expect(fetch).toHaveBeenCalledWith(`${rootUrl}/user-name`);
        expect(store.userName).toEqual(username);

        fetch.resetMocks();
    });

    it('loadTitle() should retreive and set title', async () => {
        // Arrange
        const title = 'My Title';
        const testTitle = `{ "title": "${title}" }`;
        fetch.mockResponse(testTitle);

        const store = new Store();

        // Act
        await store.loadTitle();

        // Assert
        expect(fetch).toHaveBeenCalled();
        expect(fetch).toHaveBeenCalledWith(`${rootUrl}/title`);
        expect(store.title).toEqual(title);

        fetch.resetMocks();
    });

    function buildRequestObject(method: string, body: string) {
        return { body, method };
    }
});