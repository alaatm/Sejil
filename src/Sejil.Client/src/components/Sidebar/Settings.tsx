import React, { useEffect, useState } from 'react';
import { Select, Typography } from 'antd';
import api from '../../api';
import './Settings.css'

const Settings = () => {
    const [minLogLevel, setMinLogLevel] = useState<string>();

    useEffect(() => {
        const fetchData = async () => {
            setMinLogLevel(await api.getMinLogLevel());
            document.title = await api.getTitle();
        };

        fetchData();
    }, []);

    const handleMinLogLevelChange = async (value: string) => {
        await api.setMinLogLevel(value);
        setMinLogLevel(value);
    };

    return (
        <div className="settings">
            <Typography.Title level={5}>Settings</Typography.Title>
            <div className="min-log-level">
                <Typography.Text>Minimum Log Level:</Typography.Text>
                <Select value={minLogLevel} onChange={handleMinLogLevelChange}>
                    <Select.Option key="verbose" value="verbose">Verbose</Select.Option>
                    <Select.Option key="debug" value="debug">Debug</Select.Option>
                    <Select.Option key="information" value="information">Information</Select.Option>
                    <Select.Option key="warning" value="warning">Warning</Select.Option>
                    <Select.Option key="error" value="error">Error</Select.Option>
                    <Select.Option key="critical" value="critical">Critical</Select.Option>
                </Select>
            </div>
        </div>
    );
};

export default Settings;
