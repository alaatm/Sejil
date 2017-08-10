// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import * as React from 'react';
import * as AntInput from 'antd/lib/input';
import * as AntButton from 'antd/lib/button';
import * as AntDatePicker from 'antd/lib/date-picker';
import * as AntLocaleProvider from 'antd/lib/locale-provider';
import * as AntenUS from 'antd/lib/locale-provider/en_US';
import { Moment } from 'moment';

import { action } from 'mobx';
import { inject, observer } from 'mobx-react';
import Store from '../Store';

interface IProps {
    store?: Store;
}

interface IState {
    activeFilterPeriod: string;
    periodFilterRange: Moment[];
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
        this.state = { activeFilterPeriod: '', periodFilterRange: [] };
    }

    onKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
        const store = this.props.store || new Store();

        if (e.keyCode === 13) {
            e.preventDefault();
            store.reset();
        }
    }

    onSetPeriodClick(period: string) {
        this.setPeriodFilter(period, []);
    }

    onPeriodRangeChange(date: Moment[]) {
        this.setPeriodFilter('', date);
    }

    onClearPeriodClick() {
        this.setPeriodFilter('', []);
    }

    @action setPeriodFilter(period: string, range: Moment[]) {
        this.setState({
            activeFilterPeriod: period,
            periodFilterRange: range
        });

        const store = this.props.store || new Store();
        store.dateFilter = period.length
            ? period
            : range.length
                ? range.map(m => m.toDate())
                : null;
        store.reset();
    }

    @action updateFilterText(e: React.ChangeEvent<HTMLInputElement>) {
        const store = this.props.store || new Store();
        store.queryText = e.target.value;
    }

    render() {
        const store = this.props.store || new Store();

        const LocaleProvider = AntLocaleProvider as any; // To bypass compiler error
        const enUS = AntenUS as any; // To bypass compiler error
        const Input = AntInput as any; // To bypass compiler error
        const Button = AntButton as any; // To bypass compier error
        const DatePicker = AntDatePicker as any; // To bypass compier error

        const ButtonGroup = Button.Group;
        const RangePicker = DatePicker.RangePicker;

        return (
            <LocaleProvider locale={enUS}>
                <div className="filter-bar">
                    <Input
                        className="filter-text"
                        placeholder="Type a filter then hit enter to filter the logs"
                        spellCheck={false}
                        value={store.queryText}
                        onKeyDown={this.onKeyDown}
                        onChange={this.updateFilterText} />

                    <div className="period-filter">
                        <div className="left">
                            <ButtonGroup className="filter-group">
                                {['5m', '15m', '1h', '6h', '12h', '24h', '2d', '5d'].map((b, i) => (
                                    this.state.activeFilterPeriod == b
                                        ? <Button key={i} type="primary" onClick={this.setPeriodFilter.bind(this, b)}>{b}</Button>
                                        : <Button key={i} onClick={this.onSetPeriodClick.bind(this, b)}>{b}</Button>
                                ))}
                            </ButtonGroup>
                        </div>
                        <div className="right">
                            <RangePicker value={this.state.periodFilterRange} onChange={this.onPeriodRangeChange} format={'DD MMM YYYY'} />
                            <Button onClick={this.onClearPeriodClick}>Clear All</Button>
                        </div>
                    </div>
                </div>
            </LocaleProvider>
        );
    }
}
