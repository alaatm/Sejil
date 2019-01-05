// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import './SideBar.css';

import * as React from 'react';

import ExceptionsFilter from './ExceptionsFilter';
import LogLevelFilter from './LogLevelFilter';
import SavedQueries from './SavedQueries';
import Settings from './Settings';
import UserInfo from './UserInfo';

export default class SideBar extends React.Component<{}, {}> {
    render() {
        return (
            <div className="side-bar">
                <LogLevelFilter />
                <ExceptionsFilter />
                <SavedQueries />
                <Settings />
				<UserInfo />
            </div>
        );
    }
}
