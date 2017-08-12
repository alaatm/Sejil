// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import * as React from 'react';

import { action } from 'mobx';
import { inject, observer } from 'mobx-react';
import Store from '../Store';
import ILogQuery from '../interfaces/ILogQuery';

interface IProps {
    store?: Store;
}

@inject('store')
@observer
export default class SavedQueries extends React.Component<IProps, {}> {
    constructor(props: IProps) {
        super(props);
    }

    @action loadQuery(q: ILogQuery) {
        const store = this.props.store || new Store();
        store.queryText = q.query;
        store.reset();
    }

    async componentDidMount() {
        const store = this.props.store || new Store();
        await store.loadQueries();
    }

    render() {
        const store = this.props.store || new Store();

        return (
            <div className="section">
                <div className="section-header">Saved Queries</div>
                {
                    store.queries.map(q => 
                        <div className="section-item" 
                             onClick={this.loadQuery.bind(this, q)}>
                             {q.name}
                        </div>)
                }
            </div>
        );
    }
}
