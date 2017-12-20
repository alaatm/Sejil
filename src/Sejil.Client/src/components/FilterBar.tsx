// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import './FilterBar.css';

import * as React from 'react';

import { Button, DatePicker, Input } from 'antd';
import { inject, observer } from 'mobx-react';

import { Moment } from 'moment';
import Store from '../Store';
import { action } from 'mobx';

interface IProps {
    store?: Store;
}

interface IState {
    activeFilterPeriod: string;
    periodFilterRange?: [Moment, Moment];
}

@inject('store')
@observer
export default class FilterBar extends React.Component<IProps, IState> {
    constructor(props: IProps) {
        super(props);
        this.onKeyDown = this.onKeyDown.bind(this);
        this.updateFilterText = this.updateFilterText.bind(this);
        this.onPeriodRangeChange = this.onPeriodRangeChange.bind(this);
        this.onClearPeriodClick = this.onClearPeriodClick.bind(this);
        this.saveFilter = this.saveFilter.bind(this);
        this.state = { activeFilterPeriod: '' };
    }

    onKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
        if (e.keyCode === 13) {
            e.preventDefault();
            this.props.store!.reset();
        }
    }

    saveFilter() {
        const filterName = prompt('Enter filter name');
        if (filterName != null && filterName.length) {
            if (this.props.store!.queries.findIndex(p => p.name === filterName) >= 0) {
                alert('Query name already exist.');
                return;
            }

            this.props.store!.saveQuery(filterName, this.props.store!.queryText);
        }
    }

    onSetPeriodClick(period: string) {
        this.setPeriodFilter(period);
    }

    onPeriodRangeChange(date: Moment[]) {
        this.setPeriodFilter('', [date[0], date[1]]);
    }

    onClearPeriodClick() {
        this.setPeriodFilter('');
    }

    @action setPeriodFilter(period: string, range?: [Moment, Moment]) {
        this.setState({
            activeFilterPeriod: period,
            periodFilterRange: range,
        });

        this.props.store!.dateFilter = period.length
            ? period
            : range
                ? range.map(m => m.toDate())
                : null;
        this.props.store!.reset();
    }

    @action updateFilterText(e: React.ChangeEvent<HTMLInputElement>) {
        this.props.store!.queryText = e.target.value;
    }

    render() {
        const store = this.props.store!;

        const ButtonGroup = Button.Group;
        const RangePicker = DatePicker.RangePicker;

        return (
            <div className="filter-bar">
                <div>
                    <Input
                        className="filter-text"
                        placeholder="Type a filter then hit enter to filter the logs"
                        spellCheck={false}
                        value={store.queryText}
                        onKeyDown={this.onKeyDown}
                        onChange={this.updateFilterText}
                    />
                    <Button
                        className="filter-save"
                        disabled={store.queryText.length === 0}
                        onClick={this.saveFilter}
                    >
                        Save
                    </Button>
                </div>

                <div className="period-filter">
                    <div className="left">
                        <ButtonGroup className="filter-group">
                            {['5m', '15m', '1h', '6h', '12h', '24h', '2d', '5d'].map((b, i) => (
                                this.state.activeFilterPeriod === b
                                    ? <Button
                                        key={i}
                                        type="primary"
                                        onClick={this.setPeriodFilter.bind(this, b)}
                                    >
                                        {b}
                                    </Button>
                                    : <Button key={i} onClick={this.onSetPeriodClick.bind(this, b)}>{b}</Button>
                            ))}
                        </ButtonGroup>
                    </div>
                    <div className="right">
                        <RangePicker
                            value={this.state.periodFilterRange}
                            onChange={this.onPeriodRangeChange}
                            format={'DD MMM YYYY'}
                        />
                        <Button onClick={this.onClearPeriodClick}>Clear All</Button>
                    </div>
                </div>
            </div>
        );
    }
}
