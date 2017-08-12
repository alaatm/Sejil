// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import * as React from 'react';
import LogLevelFilter from './LogLevelFilter';
import ExceptionsFilter from './ExceptionsFilter';
import SavedQueries from './SavedQueries';

export default class SideBar extends React.Component<{}, {}> {
    render() {
        return (
            <div className="side-bar">
                <LogLevelFilter />
                <ExceptionsFilter />
                <SavedQueries />
            </div>
        );
    }
}
