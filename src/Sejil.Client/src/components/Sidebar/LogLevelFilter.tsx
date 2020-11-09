import React, { useContext } from 'react';
import { Badge, Button, Radio, Tooltip, Typography } from 'antd';
import { CloseSquareFilled } from '@ant-design/icons';
import { AppContext } from '../../Context';
import './LogLevelFilter.css';

const LogLevelFilter = () => {
    const { state, dispatch } = useContext(AppContext);
    const { levelFilter } = state.queryFilters;

    return (
        <div className="log-level-filter">
            <Typography.Title level={5}>
                Log Level Filteration
                <Tooltip title="Clear">
                    <Button className="clear" ghost size="small" icon={<CloseSquareFilled />} onClick={() => dispatch({ type: 'CLEAR_LEVEL' })} />
                </Tooltip>
            </Typography.Title>
            <Radio.Group buttonStyle="solid" value={levelFilter} onChange={v => dispatch({ type: 'SET_QUERY_FILTER', payload: { levelFilter: v.target.value } })}>
                <Radio.Button value="Verbose"><Badge color="#d3d3d3" size="default" />Verbose</Radio.Button>
                <Radio.Button value="Debug"><Badge color="#9e9e9e" />Debug</Radio.Button>
                <Radio.Button value="Information"><Badge color="#007acc" />Information</Radio.Button>
                <Radio.Button value="Warning"><Badge color="#faad14" />Warning</Radio.Button>
                <Radio.Button value="Error"><Badge color="#ff4d4f" />Error</Radio.Button>
                <Radio.Button value="Critical"><Badge color="#d81b60" />Critical</Radio.Button>
            </Radio.Group>
        </div>
    );
};

export default LogLevelFilter;
