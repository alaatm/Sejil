// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import * as React from 'react';

import { inject, observer } from 'mobx-react';

import { ILogQuery } from '../interfaces';
import { Icon } from 'antd';
import Store from '../Store';
import { action } from 'mobx';

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
        this.props.store!.queryText = q.query;
        this.props.store!.reset();
    }

    deleteQuery(q: ILogQuery) {
        this.props.store!.deleteQuery(q);
    }

    async componentDidMount() {
        await this.props.store!.loadQueries();
    }

    render() {
        return (
            <div className="section">
                <div className="section-header">Saved Queries</div>
                {this.props.store!.queries.map((q, i) =>
                    <div
                        key={i}
                        className="section-item"
                        onClick={this.loadQuery.bind(this, q)}
                    >
                        {q.name}
                        <Icon
                            type="delete"
                            style={{ float: 'right' }}
                            title="Delete"
                            onClick={this.deleteQuery.bind(this, q)}
                        />
                    </div>)
                }
            </div>
        );
    }
}
