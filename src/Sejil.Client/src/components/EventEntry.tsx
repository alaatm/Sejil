// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import * as React from 'react';
import { formatLogEntryDate } from '../formatDate';
import EventEntryProperties from './EventEntryProperties';
import ILogEntry from '../interfaces/ILogEntry';

interface IProps {
    entry: ILogEntry;
}

interface IState {
    collapsed: boolean;
}

export default class EventEntry extends React.Component<IProps, IState> {
    constructor(props: IProps) {
        super(props);
        this.state = { collapsed: true };
        this.toggleEvent = this.toggleEvent.bind(this);
    }

    toggleEvent() {
        this.setState(prev => ({
            collapsed: !prev.collapsed,
        }));
    }

    render() {
        const entry = this.props.entry;
        const timestamp = formatLogEntryDate(entry.timestamp);
        const levelClass = `level level-${entry.level.toLowerCase()}`;
        const logClass = `log level-${entry.level.toLowerCase()} ${this.state.collapsed ? 'collapsed' : 'expanded'}`;

        return (
            <div className={logClass}>
                <div className="log-entry" onClick={this.toggleEvent}>
                    <div className="timestamp">{timestamp}</div>
                    <div className="message">
                        <span className={levelClass} title={entry.level}></span>
                        {entry.message}
                    </div>
                </div>
                {
                    !this.state.collapsed &&
                    <EventEntryProperties props={entry.properties} />
                }
                {
                    !this.state.collapsed && entry.exception &&
                    <p className="exception">{entry.exception}</p>
                }
            </div>
        );
    }
}
