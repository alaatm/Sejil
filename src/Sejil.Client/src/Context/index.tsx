import React, { createContext, useReducer } from 'react';
import { LogQueryFilter, LogQuery } from '../Models';
import { QueryFilterActions, logQueryFilterReducer } from './logQueryFilterReducer';
import { LogQueryActions, logQueryReducer } from './logQueryReducer';

type State = {
    queryFilters: LogQueryFilter;
    savedQueries: LogQuery[];
}

const initialState: State = {
    queryFilters: { queryText: undefined, dateFilter: undefined, dateRangeFilter: undefined, levelFilter: undefined, exceptionsOnly: false },
    savedQueries: [],
};

const AppContext = createContext<{ state: State, dispatch: React.Dispatch<QueryFilterActions | LogQueryActions> }>({
    state: initialState,
    dispatch: () => null
});

const mainReducer = ({ queryFilters, savedQueries }: State, action: QueryFilterActions | LogQueryActions) => ({
    queryFilters: logQueryFilterReducer(queryFilters, action as QueryFilterActions),
    savedQueries: logQueryReducer(savedQueries, action as LogQueryActions)
});

const AppProvider: React.FC = ({ children }) => {
    const [state, dispatch] = useReducer(mainReducer, initialState);

    return (
        <AppContext.Provider value={{ state, dispatch }}>
            {children}
        </AppContext.Provider>
    )
};

export { AppProvider, AppContext };