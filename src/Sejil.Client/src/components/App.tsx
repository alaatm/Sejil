// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import './App.css';

import * as React from 'react';

import EventList from './EventList';
import FilterBar from './FilterBar';
import { Provider } from 'mobx-react';
import SideBar from './SideBar';
import Header from './Header';
import Store from '../Store';

export default class App extends React.Component<{}, {}> {
    render() {
        return (
            <Provider store={new Store()}>
                <div className="wrapper">
                    <div className="header">
                         <Header />
                    </div>

                    <div className="left-pane">
                        <SideBar />
                    </div>
                    <div className="center-pane">
                        <FilterBar />

                        <div className="logs-pane">
                            <EventList />
                        </div>
                    </div>
                </div>
            </Provider>
        );
    }
}
