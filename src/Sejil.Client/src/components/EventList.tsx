// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import * as React from 'react';
import { observer, inject } from 'mobx-react';
import EventEntry from './EventEntry';
import Store from '../Store';

interface IProps {
    store?: Store;
}

@inject('store')
@observer
export default class EventList extends React.Component<IProps, {}> {
    private contentElem: HTMLDivElement;

    constructor(props: IProps) {
        super(props);
    }

    async load() {
        const store = this.props.store || new Store();
        await store.loadEvents();
    }

    render() {
        const store = this.props.store || new Store();

        return (
            <div className="stream-view">
                <div ref={(e: HTMLDivElement) => this.contentElem = e} className="content">
                    {store.logEntries.map(s => (
                        <EventEntry key={s.id} entry={s} />
                    ))}
                </div>
            </div >
        );
    }

    componentDidMount() {
        const elem = this.contentElem;
        elem.addEventListener('scroll', async () => {
            if (elem.scrollTop + elem.offsetHeight >= elem.scrollHeight) {
                await this.load();
            }
        });
    }
}
