'use client';

import { useState } from 'react';

interface ContactFormState {
  name: string;
  email: string;
  message: string;
}

const initialState: ContactFormState = { name: '', email: '', message: '' };

export default function ContactPage() {
  const [form, setForm] = useState<ContactFormState>(initialState);
  const [status, setStatus] = useState<'idle' | 'submitting' | 'success' | 'rateLimited' | 'error'>('idle');
  const [feedback, setFeedback] = useState('');

  function handleChange(
    event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setStatus('submitting');
    setFeedback('');

    try {
      const response = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5166/api'}/Contact`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            Name: form.name,
            Email: form.email,
            Message: form.message,
          }),
        }
      );

      if (response.status === 429) {
        const text = await response.text();
        setStatus('rateLimited');
        setFeedback(text || 'Too many requests. Please try again later.');
        return;
      }

      if (!response.ok) {
        setStatus('error');
        setFeedback('Something went wrong. Please try again.');
        return;
      }

      setStatus('success');
      setFeedback('Thanks for reaching out! Your message was sent.');
      setForm(initialState);
    } catch (error: unknown) {
      console.error('Contact form submission failed:', error);
      setStatus('error');
      setFeedback('Network error. Please check your connection and try again.');
    }
  }

  return (
    <main className="mx-auto max-w-xl px-6 py-12">
      <h1 className="text-3xl font-bold tracking-tight">Contact</h1>
      <p className="mt-2 text-gray-600">
        Send a message and I&apos;ll get back to you.
      </p>

      <form onSubmit={handleSubmit} className="mt-8 space-y-4">
        <div>
          <label htmlFor="name" className="block text-sm font-medium text-gray-700">
            Name
          </label>
          <input
            id="name"
            name="name"
            type="text"
            required
            value={form.name}
            onChange={handleChange}
            className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
          />
        </div>

        <div>
          <label htmlFor="email" className="block text-sm font-medium text-gray-700">
            Email
          </label>
          <input
            id="email"
            name="email"
            type="email"
            required
            value={form.email}
            onChange={handleChange}
            className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
          />
        </div>

        <div>
          <label htmlFor="message" className="block text-sm font-medium text-gray-700">
            Message
          </label>
          <textarea
            id="message"
            name="message"
            rows={5}
            required
            value={form.message}
            onChange={handleChange}
            className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
          />
        </div>

        <button
          type="submit"
          disabled={status === 'submitting'}
          className="rounded-md bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-700 disabled:opacity-50"
        >
          {status === 'submitting' ? 'Sending…' : 'Send message'}
        </button>

        {feedback && (
          <p
            role="status"
            className={
              status === 'success'
                ? 'text-sm text-green-600'
                : 'text-sm text-red-600'
            }
          >
            {feedback}
          </p>
        )}
      </form>
    </main>
  );
}