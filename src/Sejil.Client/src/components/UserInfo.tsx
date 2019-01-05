// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import * as React from 'react';

import { inject, observer } from 'mobx-react';
import Store from '../Store';

interface IProps {
    store?: Store;
}

@inject('store')
@observer
export default class UserInfo extends React.Component<IProps, {}> {
    
    async componentDidMount() {
        await this.props.store!.loadUserName();
    }

    render() {
        return (
            this.props.store!.userName !== ""
                                    ? <div className="section">
                <div className="section-header">Logged in user</div>
                <div className="section-item">
				{this.props.store!.userName}                    
                </div>
            </div>
                                    : ""			
        );
    }
}
