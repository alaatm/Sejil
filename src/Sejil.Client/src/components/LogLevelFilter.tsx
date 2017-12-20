// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import * as React from 'react';

import { inject, observer } from 'mobx-react';

import Store from '../Store';
import { action } from 'mobx';

interface IProps {
    store?: Store;
}

interface LogLevel {
    name: string;
    selected: boolean;
}

interface IState {
    levels: LogLevel[];
}

@inject('store')
@observer
export default class LogLevelFilter extends React.Component<IProps, IState> {
    constructor(props: IProps) {
        super(props);
        this.clearLevelFilter = this.clearLevelFilter.bind(this);
        this.state = {
            levels: [{
                name: 'Verbose',
                selected: false
            }, {
                name: 'Debug',
                selected: false
            }, {
                name: 'Information',
                selected: false
            }, {
                name: 'Warning',
                selected: false
            }, {
                name: 'Error',
                selected: false
            }, {
                name: 'Critical',
                selected: false
            }]
        };
    }

    @action levelFilterClick(level: LogLevel) {
        this.setState({
            levels: this.updateSelectedState(level)
        });

        const store = this.props.store || new Store();
        store.levelFilter = level.name;
        store.reset();
    }

    clearLevelFilter() {
        this.state.levels.forEach(l => l.selected = false);
        this.setState({
            levels: this.state.levels
        });

        const store = this.props.store || new Store();
        store.levelFilter = null;
        store.reset();
    }

    private updateSelectedState(level: LogLevel) {
        this.state.levels.forEach(l => l.selected = false);
        const item = this.state.levels.filter(l => l.name === level.name)[0];
        item.selected = true;

        return this.state.levels;
    }

    render() {
        return (
            <div className="section">
                <div className="section-header">Log Level Filteration</div>
                {this.state.levels.map((l, i) => (
                    <div
                        key={i}
                        className={`section-item ${l.selected ? 'selected' : ''}`}
                        onClick={this.levelFilterClick.bind(this, l)}
                    >
                        <div className={`level-indicator level-${l.name.toLowerCase()}`} />
                        {l.name}
                    </div>
                ))}
                <div className="section-item" onClick={this.clearLevelFilter}>
                    Clear
                </div>
            </div>
        );
    }
}
