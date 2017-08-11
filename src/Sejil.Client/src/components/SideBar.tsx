// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import * as React from 'react';

export default class SideBar extends React.Component<{}, {}> {
    render() {
        return (
            <div className="side-bar">
                <div className="section">
                    <div className="section-header">Log Level Filteration</div>
                    <div className="section-item">
                        <div className="level-indicator level-trace"></div>
                        Verbose
                    </div>
                    <div className="section-item">
                        <div className="level-indicator level-debug"></div>
                        Debug
                    </div>
                    <div className="section-item">
                        <div className="level-indicator level-information"></div>
                        Information
                    </div>
                    <div className="section-item">
                        <div className="level-indicator level-warning"></div>
                        Warning
                    </div>
                    <div className="section-item">
                        <div className="level-indicator level-error"></div>
                        Error
                    </div>
                    <div className="section-item">
                        <div className="level-indicator level-critical"></div>
                        Critical
                    </div>
                    <div className="section-item">
                        All
                    </div>
                </div>

                <div className="section">
                    <div className="section-header">Log Exceptions Filteration</div>
                    <div className="section-item">
                        Exceptions Only
                    </div>
                    <div className="section-item">
                        All
                    </div>
                </div>

                <div className="section">
                    <div className="section-header">Saved Queries</div>
                </div>
            </div>
        );
    }
}
