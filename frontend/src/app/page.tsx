import api from '@/lib/api';
import { Project } from '@/types';
import Link from 'next/link';

export default async function Home() {
  let projects: Project[] = [];
  
  try {
    // Fetch data from the .NET API on the server
    const response = await api.get('/Projects');
    projects = response.data;
  } catch (error) {
    console.error("Failed to fetch projects:", error);
  }

  return (
    <main className="min-h-screen p-8 max-w-5xl mx-auto">
      <header className="mb-12 text-center">
        <h1 className="text-5xl font-bold mb-4">Dennis Conner</h1>
        <p className="text-xl text-gray-600">Full-Stack Developer & IT Consultant</p>
      </header>

      <section className="mb-12">
        <h2 className="text-3xl font-semibold mb-6 border-b pb-2">Projects</h2>
        {projects.length === 0 ? (
          <p className="text-gray-500">No projects found. Add one via the API!</p>
        ) : (
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
        )}
      </section>
    </main>
  );
}