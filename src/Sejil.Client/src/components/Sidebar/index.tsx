import React from 'react';
import { Divider } from 'antd';
import Title from './Title';
import LogLevelFilter from './LogLevelFilter';
import LogExceptionFilter from './LogExceptionFilter';
import SavedQueryList from './SavedQueryList';
import Settings from './Settings';
import UserInfo from './UserInfo';
import './index.css'

const Sidebar = () => (
    <div className="sidebar">
        <Title />
        <Divider />
        <LogLevelFilter />
        <Divider />
        <LogExceptionFilter />
        <Divider />
        <SavedQueryList />
        <Divider />
        <Settings />
        <Divider />
        <UserInfo />
    </div>
);

export default Sidebar;
