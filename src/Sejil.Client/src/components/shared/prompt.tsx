import React from 'react';
import ReactDOM from "react-dom";
import { Form, Input, Modal } from 'antd';
import { Rule } from 'antd/lib/form';


type PromptProps = {
    title: string;
    placeholder: string;
    visible: boolean;
    close: (value?: string) => void;
    afterClose?: () => void;
    validators: Rule[];
}

const Prompt = (props: PromptProps) => {
    const { title, placeholder, visible, close, afterClose, validators } = props;
    const [form] = Form.useForm();

    const handleOk = async () => {
        try {
            await form.validateFields();
            close(form.getFieldValue('value'));
        } catch {
        }
    };

    return (
        <Modal
            title={title}
            visible={visible}
            onOk={handleOk}
            onCancel={() => close()}
            afterClose={afterClose}
        >
            <Form name="prompt" form={form}>
                <Form.Item name="value" rules={
                    [
                        { required: true, message: title },
                        { whitespace: true, message: title },
                        ...validators
                    ]}>
                    <Input placeholder={placeholder} />
                </Form.Item>
            </Form>
        </Modal>
    );
};

type Props = {
    title: string;
    placeholder: string;
    validators: Rule[];
}

export default function prompt(props: Props): Promise<string | undefined> {
    return new Promise(resolve => {
        const div = document.createElement('div');
        document.body.appendChild(div);
        let promptProps: PromptProps = { ...props, close, visible: true };

        function render(p: PromptProps) {
            ReactDOM.render(<Prompt {...p} />, div);
        }

        function close(value?: string) {
            promptProps = {
                ...promptProps,
                visible: false,
                afterClose: () => {
                    const unmountResult = ReactDOM.unmountComponentAtNode(div);
                    if (unmountResult && div.parentNode) {
                        div.parentNode.removeChild(div);
                    }
                    resolve(value);
                }
            };
            render(promptProps);
        }

        render(promptProps);
    });
}