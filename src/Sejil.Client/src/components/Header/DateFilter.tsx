import React from 'react';
import { Radio } from 'antd';

type Props = {
    value?: string;
    onChange: (value: string) => void;
};

const DateFilter = (props: Props) => (
    <Radio.Group value={props.value} buttonStyle="solid" size="large" onChange={e => props.onChange(e.target.value)}>
        <Radio.Button value="5m">5m</Radio.Button>
        <Radio.Button value="15m">15m</Radio.Button>
        <Radio.Button value="1h">1h</Radio.Button>
        <Radio.Button value="6h">6h</Radio.Button>
        <Radio.Button value="12h">12h</Radio.Button>
        <Radio.Button value="24h">24h</Radio.Button>
        <Radio.Button value="2d">2d</Radio.Button>
        <Radio.Button value="5d">5d</Radio.Button>
    </Radio.Group>
);

export default DateFilter;
