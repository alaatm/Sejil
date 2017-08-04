// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import * as React from 'react';
import * as ReactDOM from 'react-dom';
import * as mobx from 'mobx';
// import DevTools from 'mobx-react-devtools';
import App from './components/App';

mobx.useStrict(true);

ReactDOM.render(
    // <div> <App /><DevTools /> </div>,
    <div> <App /> </div>,
    document.getElementById('app'),
);
