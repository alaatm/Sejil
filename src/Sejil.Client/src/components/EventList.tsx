// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import './EventList.css';

import * as React from 'react';

import { inject, observer } from 'mobx-react';

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
        await this.props.store!.loadEvents();
    }

    render() {
        return (
            <div ref={(e: HTMLDivElement) => this.contentElem = e} className="logs-view">
                {this.props.store!.logEntries.map(s => (
                    <EventEntry key={s.id} entry={s} />
                ))}
            </div >
        );
    }

    async componentDidMount() {
        await this.props.store!.loadEvents();

        const elem = this.contentElem;
        elem.addEventListener('scroll', async () => {
            if (elem.scrollTop + elem.offsetHeight >= elem.scrollHeight) {
                await this.load();
            }
        });
    }
}
