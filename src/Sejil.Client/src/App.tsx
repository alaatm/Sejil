import React from 'react';
import { Layout } from 'antd';
import { AppProvider } from './Context';
import { Sidebar, Header, Content } from './components';
import './App.css';

const App = () => {
    return (
        <AppProvider>
            <Layout>
                <Layout.Sider width={250}><Sidebar /></Layout.Sider>
                <Layout>
                    <Layout.Header><Header /></Layout.Header>
                    <Layout.Content><Content /></Layout.Content>
                </Layout>
            </Layout>
        </AppProvider>
    );
}

export default App;
