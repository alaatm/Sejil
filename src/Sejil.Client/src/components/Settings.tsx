// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import * as React from 'react';
import { Select } from 'antd';
import { inject, observer } from 'mobx-react';
import Store from '../Store';

const Option = Select.Option;

interface IProps {
    store?: Store;
}

@inject('store')
@observer
export default class Settings extends React.Component<IProps, {}> {
    private _logLevels = ['Verbose', 'Debug', 'Information', 'Warning', 'Error', 'Critical'];

    async componentDidMount() {
        await this.props.store!.loadMinLogLevel();
    }

    handleMinLogLevelChange = async (value: string) => {
        await this.props.store!.setMinLogLevel(value);
    }

    render() {
        return (
            <div className="section">
                <div className="section-header">Settings</div>
                <div className="section-item">
                    Minimum Log Level
                    <Select
                        value={this.props.store!.minLogLevel}
                        onChange={this.handleMinLogLevelChange}
                        style={{ width: '100%' }}
                    >
                        {this._logLevels.map((l, i) => <Option key={i} value={l}>{l}</Option>)}
                    </Select>
                </div>
            </div>
        );
    }
}
