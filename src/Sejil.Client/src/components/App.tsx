// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.


import * as React from 'react';
import { Provider } from 'mobx-react';
import Store from '../Store';
import EventList from './EventList';
import FilterBar from './FilterBar';
import SideBar from './SideBar';

export default class App extends React.Component<{}, {}> {
    render() {
        return (
            <Provider store={new Store()}>
                <div>
                    <div className="header">
                        Sejil
                    </div>
                    <div className="view">
                        <div className="events">
                            <SideBar />
                            <FilterBar />
                            <EventList />
                        </div>
                    </div>
                </div>
            </Provider>
        );
    }
}
