// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import * as React from 'react';

import { ILogEntryProperty } from '../interfaces';

interface IProps {
    props: ILogEntryProperty[];
}

export default class EventEntry extends React.Component<IProps, {}> {
    constructor(props: IProps) {
        super(props);
    }

    render() {
        return (
            <div className="log-properties">
                {this.props.props.map(p => (
                    <div key={p.id} className="property">
                        <div className="property-name">{p.name}</div>
                        <div className="property-value">{p.value || ' '}</div>
                    </div>
                ))}
            </div>
        );
    }
}
