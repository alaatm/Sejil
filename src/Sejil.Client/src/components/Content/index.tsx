import React, { useState, useEffect, useContext } from 'react';
import { List, notification } from 'antd';
import { InfiniteScroll } from '../shared';
import Entry from './Entry';
import { LogEntry } from '../../Models';
import { AppContext } from '../../Context';
import api from '../../api';
import './index.css';

const Content = () => {
    const { queryFilters } = useContext(AppContext).state;

    const [logs, setLogs] = useState<LogEntry[]>([]);
    const [page, setPage] = useState(2);
    const [loading, setLoading] = useState(false);
    const [hasMore, setHasMore] = useState(false);
    const [startingTimestamp, setStartingTimestamp] = useState<string>();

    useEffect(() => {
        const fetchData = async () => {
            setLogs([]);
            setPage(2);
            setLoading(true);
            const { error, events } = await api.getLogEvents(1, queryFilters);
            if (error) {
                notification.error({ message: 'Invalid Query', description: error });
            } else {
                setLogs(events);
                setHasMore(events.length > 0);
                setStartingTimestamp(events[0]?.timestamp);
            }
            setLoading(false);
        };

        fetchData();
    }, [queryFilters]);

    const loadMore = async () => {
        setLoading(true);
        const { events } = await api.getLogEvents(page, queryFilters, startingTimestamp);
        setLogs(logs => [...logs, ...events]);
        setHasMore(events.length > 0);
        setLoading(false);
        setPage(page => page + 1);
    };

    return (
        <div className="content">
            <InfiniteScroll
                initialLoad={false}
                loadMore={loadMore}
                hasMore={!loading && hasMore}
                threshold={200}
                useWindow={false}
            >
                <List
                    size="small"
                    loading={loading}
                    dataSource={logs}
                    renderItem={item => (
                        <List.Item key={item.id}>
                            <Entry item={item} />
                        </List.Item>
                    )}
                />
            </InfiniteScroll>
        </div>
    );
};

export default Content;
