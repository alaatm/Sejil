import * as React from 'react';
import * as Input from 'antd/lib/input';
import { action } from 'mobx';
import { inject, observer } from 'mobx-react';
import Store from '../Store';

interface IProps {
    store?: Store;
}

@inject('store')
@observer
export default class FilterBar extends React.Component<IProps, {}> {
    constructor(props: IProps) {
        super(props);
        this.onKeyDown = this.onKeyDown.bind(this);
        this.updateFilterText = this.updateFilterText.bind(this);
    }

    onKeyDown(e: React.KeyboardEvent<HTMLTextAreaElement>) {
        const store = this.props.store || new Store();

        if (e.shiftKey && e.keyCode === 13) {
            e.preventDefault();
            store.filterEvents();
        }
    }

    @action updateFilterText(e: React.ChangeEvent<HTMLInputElement>) {
        const store = this.props.store || new Store();
        store.queryText = e.target.value;
    }

    render() {
        const store = this.props.store || new Store();
        const TextArea = (Input as any).TextArea;

        return (
            <div className="filter-bar">
                <TextArea
                    spellCheck={false}
                    rows={3}
                    value={store.queryText}
                    onKeyDown={this.onKeyDown}
                    onChange={this.updateFilterText} />
            </div>
        );
    }
}
