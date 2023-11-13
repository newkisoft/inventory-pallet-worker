pipeline {
    agent any

    environment {
        SOLUTION_PATH = 'worker.csproj'
        ANGULAR_PATH = 'frontend' // Path to the Angular project
    }

    stages {
        stage('Clean Workspace') {
            steps {
                deleteDir() // Clean the workspace
            }
	}


        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Create production branch') {
             steps {
                script {
                    try {
                        sh "git switch -c production"
                    } catch (Exception e) {
                        echo "An error occurred: ${e.getMessage()}"
                    }
                }
            }
        }
        
        stage('Switch to production branch')
        {
           steps {
                sh "git switch production"
		sh "git reset --hard"
                sh "git pull origin main"
           }
        }

        stage('Restore Dependencies') {
            steps {
                sh "dotnet restore ${SOLUTION_PATH}"
            }
        }

        stage('Build .NET') {
            steps {
                sh "dotnet build ${SOLUTION_PATH}"
            }
        }

        stage('Test .NET') {
            steps {
                sh "dotnet test ${SOLUTION_PATH}"
            }
        }

        stage('Publish .NET') {
            steps {
                sh "dotnet publish ${SOLUTION_PATH}"
            }
        }

      
        stage('Push to production branch') {
            steps {
                sh "rm -R Web/"
                sh "rm -R Services/"
                sh "git add ."
                sh "git commit -m \"Jenkins build and deploy\""
                sh "git push -f origin production"
            }
        }
    }

    post {
        always {
            script {
                sh "git stash" // Stash changes
                sh "git clean -df" // Discard untracked files
                sh "git switch main"
                sh "git reset --hard"
            }
        }
    }
}

