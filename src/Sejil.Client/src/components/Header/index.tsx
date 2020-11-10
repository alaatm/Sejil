import React, { useState, useContext, useEffect } from 'react';
import dayjs from 'dayjs';
import { Button, Col, Input, Row } from 'antd';
import { DatePicker, prompt } from '../shared';
import DateFilter from './DateFilter';
import { AppContext } from '../../Context';
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
        setCanSave(queryText !== undefined);
    }, [queryText]);


    useEffect(() => {
        setCanClear(queryText !== undefined || dateFilter !== undefined || dateRangeFilter !== undefined);
    }, [queryText, dateFilter, dateRangeFilter]);

    const handleQuerySave = async () => {
        const value = await prompt({ title: 'Please enter query name', placeholder: 'Query name' });
        if (value) {
            const query = { name: value, query: queryText! };
            await api.saveQuery(query);
            dispatch({ type: 'SAVE_QUERY', payload: query });
        }
    };

    const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setQuery(e.target.value);
        setCanSave(e.target.value === queryText);
    };

    const handleSearchClick = (value: string) => {
        dispatch({ type: 'SET_QUERY_FILTER', payload: { queryText: value.trim() ? value.trim() : undefined } });
    };

    const handleDateRangeFilterChange = (values: [dayjs.Dayjs, dayjs.Dayjs] | undefined) => {
        if (values && values[0] && values[1]) {
            dispatch({ type: 'SET_QUERY_FILTER', payload: { dateRangeFilter: values, dateFilter: undefined } });
        }
    };

    return (
        <div className="header">
            <Row>
                <Input.Search className={`query${canSave ? ' active' : ''}`} value={query} type="text" size="large" placeholder="Type a filter then hit enter to filter the logs"
                    onChange={handleSearchChange}
                    onSearch={handleSearchClick} />
                <Button className="save" size="large" type="primary" disabled={!canSave} onClick={handleQuerySave}>Save</Button>
            </Row>
            <Row>
                <Col span={12}>
                    <DateFilter value={dateFilter} onChange={value => dispatch({ type: 'SET_QUERY_FILTER', payload: { dateFilter: value, dateRangeFilter: undefined } })} />
                </Col>
                <Col className="float-right" span={12}>
                    {/* 
                    // @ts-ignore */}
                    <DatePicker.RangePicker value={dateRangeFilter} size="large" onChange={handleDateRangeFilterChange} />
                    <Button className="clear" size="large" type="primary" danger disabled={!canClear} onClick={() => dispatch({ type: 'CLEAR_QUERY_AND_DATE' })}>Clear</Button>
                </Col>
            </Row>
        </div>
    );
};

export default Header;