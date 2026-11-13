import { Suspense } from 'react';
import { cacheLife } from 'next/cache';
import api from '@/lib/api';
import type { Project } from '@/types';
import Link from 'next/link';

// Static shell renders immediately
export default function Home() {
  return (
    <main className="min-h-screen p-8 max-w-5xl mx-auto">
      <header className="mb-12 text-center">
        <h1 className="text-5xl font-bold mb-4">Dennis Conner</h1>
        <p className="text-xl text-gray-600">Full-Stack Developer & IT Consultant</p>
      </header>

      <section className="mb-12">
        <h2 className="text-3xl font-semibold mb-6 border-b pb-2">Projects</h2>
        <Suspense fallback={<p className="text-gray-500">Loading projects...</p>}>
          <ProjectsList />
        </Suspense>
      </section>
    </main>
  );
}

// Cached data fetch: runs at build time cleanly, no error spam if API is down
async function getProjects(): Promise<Project[]> {
  'use cache';
  cacheLife('hours');

  try {
    const response = await api.get<Project[]>('/Projects');
    return response.data;
  } catch {
    return [];
  }
}

// Dynamic component: renders cached projects, streams in
async function ProjectsList() {
  const projects = await getProjects();

  if (projects.length === 0) {
    return <p className="text-gray-500">No projects found. Add one via the API!</p>;
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      {projects.map((project) => (
        <div key={project.id} className="border rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow">
          <h3 className="text-2xl font-bold mb-2">{project.title}</h3>
          <p className="text-gray-700 mb-4">{project.description}</p>
          <p className="text-sm text-blue-600 font-mono mb-4">{project.techStack}</p>
          <Link
            href={`/projects/${project.id}`}
            className="text-white bg-black px-4 py-2 rounded hover:bg-gray-800"
          >
            View Details
          </Link>
        </div>
      ))}
    </div>
  );
}