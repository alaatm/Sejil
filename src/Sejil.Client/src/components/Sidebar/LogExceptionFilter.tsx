import React, { useContext } from 'react';
import { Button, Radio, Tooltip, Typography } from 'antd';
import { CloseSquareFilled } from '@ant-design/icons';
import { AppContext } from '../../Context';

const LogExceptionFilter = () => {
    const { state, dispatch } = useContext(AppContext);
    const { exceptionsOnly } = state.queryFilters;

    return (
        <div className="log-exception-filter">
            <Typography.Title level={5}>
                Log Exceptions Filteration
                <Tooltip title="Clear">
                    <Button className="clear" ghost size="small" icon={<CloseSquareFilled />} onClick={() => dispatch({ type: 'CLEAR_EX' })} />
                </Tooltip>
            </Typography.Title>
            <Radio.Group buttonStyle="solid" value={exceptionsOnly ? 'true' : 'false'} onChange={v => dispatch({ type: 'SET_QUERY_FILTER', payload: { exceptionsOnly: true } })}>
                <Radio.Button value="true">Exceptions Only</Radio.Button>
            </Radio.Group>
        </div>
    );
};

export default LogExceptionFilter;
