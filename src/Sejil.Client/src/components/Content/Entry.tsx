import React, { useState } from 'react';
import { CSSTransition } from 'react-transition-group';
import { Badge } from 'antd';
import { LogEntry } from '../../Models';
import { formatDate, levelToColor, levelMap } from './utils';
import './Entry.css';

type Props = {
    item: LogEntry;
}

const Entry = (props: Props) => {
    const { item } = props;
    const [expanded, setExpanded] = useState(false);

    const formatMessage = () => item.spans.map(s => (
        s.kind === null
            ? s.text
            : <span className={`prop-value ${s.kind}`}>{s.text}</span>
    ));

    return (
        <div className={`log-entry level-${levelMap[item.level]}${expanded ? ' expanded' : ''}`} onClick={() => setExpanded(!expanded)}>
            <div className="summary">
                <div className="date">{formatDate(item.timestamp)}</div>
                <div className="message"><Badge color={levelToColor(item.level)} size="default" />{formatMessage()}</div>
            </div>
            <CSSTransition in={expanded} className="details" timeout={200} unmountOnExit>
                <div className="details">
                    <div className="properties">
                        {item.properties.map(p => (
                            <div key={p.id} className="property">
                                <div className="name">{p.name}</div>
                                <div className="value">{p.value}</div>
                            </div>
                        ))}
                    </div>
                    {item.exception && <div className="exception">{item.exception}</div>}
                </div>
            </CSSTransition>
        </div>
    );
};

export default Entry;
