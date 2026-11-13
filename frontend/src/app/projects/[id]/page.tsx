import { Suspense } from 'react';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import api from '@/lib/api';
import type { Project } from '@/types';

interface ViewsResponse {
  ProjectId: number;
  Views: number;
}

async function getProject(id: string): Promise<Project | null> {
  try {
    const response = await api.get<Project>(`/Projects/${id}`);
    return response.data;
  } catch (error: unknown) {
    console.error(`Failed to fetch project ${id}:`, error);
    return null;
  }
}

async function getViews(id: string): Promise<number> {
  try {
    const response = await api.get<ViewsResponse>(`/Projects/${id}/views`);
    return response.data.Views;
  } catch (error: unknown) {
    console.error(`Failed to fetch views for project ${id}:`, error);
    return 0;
  }
}

async function ProjectDetail({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;

  const project = await getProject(id);
  if (!project) {
    notFound();
  }

  const views = await getViews(id);

  return (
    <article className="mt-6">
      <h1 className="text-3xl font-bold tracking-tight">{project.title}</h1>

      <p className="mt-2 text-sm text-gray-500">
        {views} {views === 1 ? 'view' : 'views'}
      </p>

      {project.imageUrl && (
        <img
          src={project.imageUrl}
          alt={project.title}
          className="mt-6 w-full rounded-lg border object-cover"
        />
      )}

      <p className="mt-6 whitespace-pre-line text-gray-800">
        {project.description}
      </p>

      {project.techStack && (
        <section className="mt-6">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-500">
            Tech Stack
          </h2>
          <p className="mt-1 text-gray-800">{project.techStack}</p>
        </section>
      )}

      {project.gitHubUrl && (
        <a
          href={project.gitHubUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="mt-6 inline-block rounded-md bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-700"
        >
          View on GitHub
        </a>
      )}
    </article>
  );
}

export default function ProjectDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  return (
    <main className="mx-auto max-w-4xl px-6 py-12">
      <Link href="/" className="text-sm text-blue-600 hover:underline">
        &larr; Back to projects
      </Link>

      <Suspense fallback={<p className="mt-6 text-gray-500">Loading project…</p>}>
        <ProjectDetail params={params} />
      </Suspense>
    </main>
  );
}