import * as React from 'react';
import dateFormat from '../dateFormat';
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
        const timestamp = dateFormat(entry.timestamp);
        const levelClass = `level-indicator level-${entry.level.toLowerCase()}`;
        const eventClass = `event level-${entry.level.toLowerCase()} ${this.state.collapsed ? 'collapsed' : 'expanded'}`;

        return (
            <div className={eventClass} onClick={this.toggleEvent}>
                <div className="timestamp">{timestamp}</div>
                <div className="description">
                    <div className="message">
                        <span className={levelClass} title={entry.level}></span>
                        {entry.message}
                        {!this.state.collapsed && <EventEntryProperties props={entry.properties} />}
                    </div>
                    {
                        !this.state.collapsed && entry.exception &&
                        <p className="exception">{entry.exception}</p>
                    }
                </div>
            </div>
        );
    }
}
