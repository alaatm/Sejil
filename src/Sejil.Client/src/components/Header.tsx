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
export default class Header extends React.Component<IProps, {}> {
    
    async componentDidMount() {
        await this.props.store!.loadTitle();
		 document.title = this.props.store!.title;
    }

    render() {
        return (
            this.props.store!.title		
        );
    }
}
