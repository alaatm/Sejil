import * as React from 'react';
import IEventEntryProperty from '../interfaces/IEventEntryProperty';

interface IProps {
    props: IEventEntryProperty[];
}

export default class EventEntry extends React.Component<IProps, {}> {
    constructor(props: IProps) {
        super(props);
    }

    render() {
        return (
            <div className="event-properties">
                {this.props.props.map(p => (
                    <div key={p.id} className="property">
                        <div className="property-name">{p.key}</div>
                        <div className="property-value">{p.value || ' '}</div>
                    </div>
                ))}
            </div>
        );
    }
}
