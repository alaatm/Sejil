import React, { useEffect, useState } from 'react';
import { Divider, Typography } from 'antd';
import { UserOutlined } from '@ant-design/icons';
import api from '../../api';
import './UserInfo.css';

const UserInfo = () => {
    const [user, setUser] = useState<string>();

    useEffect(() => {
        const fetchData = async () => setUser(await api.getLoggedInUser());
        fetchData();
    }, []);

    return user ? (
        <>
            <div className="user-info">
                <Typography.Title level={5}>Logged in as</Typography.Title>
                <span><UserOutlined /> {user}</span>
            </div>
            <Divider />
        </>
    ) : null;
};

export default UserInfo;
