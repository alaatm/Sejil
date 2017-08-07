// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import ILogEntry from '../src/interfaces/ILogEntry';
import ILogEntryProperty from '../src/interfaces/ILogEntryProperty';
import ILogQuery from '../src/interfaces/ILogQuery';

describe('none', () => {
    it('none', () => { });
});

export function createTestLogEntries(start: number, end: number) {
    const events: ILogEntry[] = [];
    
    for (let i = start; i < end; i++) {
        const id = pad(i, 2);
        events.push({
            id: id,
            message: `msg ${i}`,
            messageTemplate: `msg ${i}`,
            level: 'info',
            timestamp: hoursFromNow(i-200).toString(),
            properties: createLogProperties(id, 3)
        });
    }

    return {
        events,
        eventsJson: JSON.stringify(events)
    };
}

export function createTestLogQueries() {
    const queries: ILogQuery[] = [];
    
    for (let i = 0; i < 10; i++) {
        queries.push({
            name: `q${i}`,
            query: `query${i}`
        });
    }

    return {
        queries,
        queriesJson: JSON.stringify(queries)
    };
}

export function createTestQuery(name = 'query', query = 'p=v'): ILogQuery {
    return {
        name,
        query
    };
}

function createLogProperties(logId: string, count: number) {
    let props: ILogEntryProperty[] = [];
    for (let i = 0; i < count; i++) {
        props.push({
            id: i,
            logId,
            name: `name ${i}`,
            value: `value ${i}` 
        });
    }

    return props;
}

function pad(num: number, size: number) {
    const s = '000000000' + num;
    return s.substr(s.length-size);
}

function hoursFromNow(h: number) {
  let d = new Date();
  d.setHours(d.getHours() + h);
  return d;
}