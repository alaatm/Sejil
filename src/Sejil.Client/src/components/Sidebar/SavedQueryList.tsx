import React, { useContext, useEffect } from 'react';
import { Button, Modal, Tooltip, Typography } from 'antd';
import { InfoCircleFilled, DeleteFilled } from '@ant-design/icons';
import { AppContext } from '../../Context';
import api from '../../api';
import './SavedQueryList.css'

const SavedQueryList = () => {
    const { state, dispatch } = useContext(AppContext);
    const { savedQueries } = state;

    useEffect(() => {
        const fetchData = async () => {
            const savedQueries = await api.getSavedQueries();
            if (savedQueries.length) {
                dispatch({ type: 'INITIALIZE_SAVED_QUERIES', paylod: savedQueries });
            }
        };

        fetchData();
    }, [dispatch]);

    const handleDeleteSavedQuery = (name: string) => {
        Modal.confirm({
            title: `Delete '${name}' query?`,
            onOk: async () => {
                await api.deleteQuery(name);
                dispatch({ type: 'DELETE_QUERY', payload: name });
            },
        });
    };

    return (
        <div className="saved-query-list">
            <Typography.Title level={5}>Saved Queries</Typography.Title>
            {savedQueries.map(p => (
                <div className="item" key={p.name}>
                    <Typography.Link className="name" onClick={() => dispatch({ type: 'SET_QUERY_FILTER', payload: { queryText: p.query } })}>{p.name}</Typography.Link>
                    <Tooltip title="Delete">
                        <Button className="delete" ghost size="small" icon={<DeleteFilled />} onClick={() => handleDeleteSavedQuery(p.name)} />
                    </Tooltip>
                </div>
            ))}
            {savedQueries.length === 0 && (
                <span className="empty">
                    <InfoCircleFilled /> No saved queries found!
                </span>
            )}
        </div>
    )
};

export default SavedQueryList;
