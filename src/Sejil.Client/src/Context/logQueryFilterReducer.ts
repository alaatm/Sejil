import { LogQueryFilter } from '../Models';

export type QueryFilterActions =
    | { type: 'SET_QUERY_FILTER', payload: Partial<LogQueryFilter> }
    | { type: 'CLEAR_LEVEL' }
    | { type: 'CLEAR_EX' }
    | { type: 'CLEAR_QUERY_AND_DATE' }
    | { type: 'SET_PAGE', payload: number };

export const logQueryFilterReducer = (state: LogQueryFilter, action: QueryFilterActions): LogQueryFilter => {
    switch (action.type) {
        case 'SET_QUERY_FILTER':
            return { ...state, ...action.payload };
        case 'CLEAR_LEVEL':
            return { ...state, levelFilter: undefined };
        case 'CLEAR_EX':
            return { ...state, exceptionsOnly: false };
        case 'CLEAR_QUERY_AND_DATE':
            return { ...state, queryText: undefined, dateFilter: undefined, dateRangeFilter: undefined };
        default:
            return state;
    }
};