import { LogQuery } from '../Models';

export type LogQueryActions =
    | { type: 'INITIALIZE_SAVED_QUERIES', paylod: LogQuery[] }
    | { type: 'SAVE_QUERY', payload: LogQuery }
    | { type: 'DELETE_QUERY', payload: string }

export const logQueryReducer = (state: LogQuery[], action: LogQueryActions): LogQuery[] => {
    switch (action.type) {
        case 'INITIALIZE_SAVED_QUERIES':
            return [...action.paylod];
        case 'SAVE_QUERY':
            return [...state, action.payload];
        case 'DELETE_QUERY':
            return [...state.filter(p => p.name !== action.payload)]
        default:
            return state;
    }
};