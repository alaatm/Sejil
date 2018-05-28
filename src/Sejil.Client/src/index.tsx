import './index.css';

import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { configure } from 'mobx';

import App from './components/App';

configure({ enforceActions: true });

let mobxDevTools: JSX.Element | boolean = false;
if (process.env.NODE_ENV === 'development') {
    const DevTools = require('mobx-react-devtools').default;
    mobxDevTools = <DevTools />;
}

ReactDOM.render(
    <div>{mobxDevTools}<App /></div>,
    document.getElementById('root') as HTMLElement
);
