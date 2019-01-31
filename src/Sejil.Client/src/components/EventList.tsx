// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import './EventList.css';

import * as React from 'react';

import { inject, observer } from 'mobx-react';
import { notification } from 'antd';

import EventEntry from './EventEntry';
import Loader from './Loader';
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
        this.props.store!.onEventsLoadError = (err => {
            let message = '';
            let description = '';

            if (typeof err === 'string') {
                message = 'Network Error';
                description = err;
            } else {
                message = 'Server Error';
                description = `Unable to fetch log events due to server error: ${err.statusText}`;
            }

            notification.error({
                message,
                description,
                duration: null,
            });
        });
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
                {this.props.store!.loading && <Loader />}
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
