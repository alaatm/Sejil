import React, { useState, useContext, useEffect } from 'react';
import dayjs from 'dayjs';
import { Button, Col, Input, Row } from 'antd';
import { DatePicker, prompt } from '../shared';
import DateFilter from './DateFilter';
import { AppContext } from '../../Context';
import { isEnterKey } from './utils';
import api from '../../api';
import './index.css';

const Header = () => {
    const { state, dispatch } = useContext(AppContext);
    const { queryText, dateFilter, dateRangeFilter } = state.queryFilters;

    const [query, setQuery] = useState(queryText);
    const [canSave, setCanSave] = useState(false);
    const [canClear, setCanClear] = useState(queryText !== undefined || dateFilter !== undefined || dateRangeFilter !== undefined);

    useEffect(() => {
        setQuery(queryText);
        setCanClear(true);
    }, [queryText]);

    const handleQuerySave = async () => {
        const value = await prompt({ title: 'Please enter query name', placeholder: 'Query name' });
        if (value) {
            const query = { name: value, query: queryText! };
            await api.saveQuery(query);
            dispatch({ type: 'SAVE_QUERY', payload: query });
            setCanClear(true);
        }
    };

    const handleQueryTextChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setCanSave(e.target.value ? true : false);
        setQuery(e.target.value);
    };

    const handleQueryTextInputKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (isEnterKey(e)) {
            e.preventDefault();
            dispatch({ type: 'SET_QUERY_FILTER', payload: { queryText: query } });
            setCanClear(true);
        }
    };

    const handleDateFilterChange = (value: string) => {
        dispatch({ type: 'SET_QUERY_FILTER', payload: { dateFilter: value, dateRangeFilter: undefined } });
        setCanClear(true);
    };

    const handleDateRangeFilterChange = (values: [dayjs.Dayjs, dayjs.Dayjs] | undefined) => {
        if (values && values[0] && values[1]) {
            dispatch({ type: 'SET_QUERY_FILTER', payload: { dateRangeFilter: values, dateFilter: undefined } });
            setCanClear(true);
        }
    };

    const handleClearClick = () => {
        setQuery(undefined);
        dispatch({ type: 'CLEAR_QUERY_AND_DATE' });
        setCanClear(false);
    };

    return (
        <div className="header">
            <Row>
                <Input className="query" value={query} type="text" size="large" placeholder="Type a filter then hit enter to filter the logs" onChange={handleQueryTextChange} onKeyDown={handleQueryTextInputKeyDown} />
                <Button size="large" type="primary" disabled={!canSave} onClick={handleQuerySave}>Save</Button>
            </Row>
            <Row>
                <Col span={12}>
                    <DateFilter value={dateFilter} onChange={value => handleDateFilterChange(value)} />
                </Col>
                <Col className="float-right" span={12}>
                    {/* 
                    // @ts-ignore */}
                    <DatePicker.RangePicker value={dateRangeFilter} size="large" onChange={handleDateRangeFilterChange} />
                    <Button size="large" type="primary" danger disabled={!canClear} onClick={handleClearClick}>Clear</Button>
                </Col>
            </Row>
        </div>
    );
};

export default Header;