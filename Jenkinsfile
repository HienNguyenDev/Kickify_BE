pipeline {
    agent any
    
    environment {
        DOCKER_IMAGE = 'kickify-api'
        CONTAINER_NAME = 'kickify-api'
        DOCKERFILE_PATH = 'Kickify.Api/Dockerfile'
        DOCKER_COMPOSE_FILE = 'docker-compose.yml'
        ENV_FILE = '/var/lib/jenkins/secrets/.env.production'
    }
    
    stages {
        stage('Cleanup Workspace') {
            steps {
                echo 'Cleaning workspace...'
                cleanWs()
            }
        }
        
        stage('Checkout Code') {
            steps {
                echo 'Checking out code from GitHub...'
                checkout scm
            }
        }
        
        stage('Verify Environment') {
            steps {
                echo 'Verifying environment...'
                sh '''
                    echo "Current directory: $(pwd)"
                    echo "Branch: ${GIT_BRANCH}"
                    echo "Build number: ${BUILD_NUMBER}"
                    
                    echo "Checking Docker..."
                    docker --version
                    docker-compose --version
                    
                    echo "Checking required files..."
                    if [ ! -f ${DOCKERFILE_PATH} ]; then
                        echo "ERROR: Dockerfile not found at ${DOCKERFILE_PATH}"
                        exit 1
                    fi
                    
                    if [ ! -f ${DOCKER_COMPOSE_FILE} ]; then
                        echo "ERROR: docker-compose.yml not found"
                        exit 1
                    fi
                    
                    if [ ! -f ${ENV_FILE} ]; then
                        echo "ERROR: Secrets file not found at ${ENV_FILE}"
                        echo "Please create the secrets file on the server first"
                        exit 1
                    fi
                    
                    echo "All checks passed"
                '''
            }
        }
        
        stage('Build Docker Image') {
            steps {
                echo 'Building Docker image...'
                sh '''
                    docker build \
                        -f ${DOCKERFILE_PATH} \
                        -t ${DOCKER_IMAGE}:${BUILD_NUMBER} \
                        -t ${DOCKER_IMAGE}:latest \
                        .
                    
                    echo "Image built successfully"
                    docker images | grep ${DOCKER_IMAGE}
                '''
            }
        }
        
        stage('Stop Old Containers') {
            steps {
                echo 'Stopping old containers...'
                sh '''
                    docker-compose -f ${DOCKER_COMPOSE_FILE} down || true
                    docker stop ${CONTAINER_NAME} || true
                    docker rm ${CONTAINER_NAME} || true
                    echo "Old containers stopped"
                '''
            }
        }
        
        stage('Deploy with Docker Compose') {
            steps {
                echo 'Deploying application...'
                sh '''
                    export BUILD_NUMBER=${BUILD_NUMBER}
                    docker-compose -f ${DOCKER_COMPOSE_FILE} up -d
                    echo "Deployment completed"
                '''
            }
        }

        stage('Update Nginx Upstream') {
            steps {
                echo 'Updating Nginx upstream for current container IP...'
                sh '''
                    set -euo pipefail

                    echo "Jenkins runtime info:"
                    whoami
                    hostname

                    API_IP=$(docker inspect -f '{{range.NetworkSettings.Networks}}{{.IPAddress}}{{end}}' ${CONTAINER_NAME})
                    if [ -z "$API_IP" ]; then
                        echo "ERROR: Could not resolve container IP for ${CONTAINER_NAME}"
                        exit 1
                    fi

                    sudo tee /etc/nginx/conf.d/upstream-kickify.conf > /dev/null <<EOF
upstream kickify_api {
    server ${API_IP}:8080 max_fails=2 fail_timeout=5s;
    keepalive 64;
}
EOF

                    sudo nginx -t
                    sudo systemctl reload nginx
                    echo "Nginx upstream updated to ${API_IP}:8080"
                '''
            }
        }
        
        stage('Verify Deployment') {
            steps {
                echo 'Verifying deployment...'
                sh '''
                    echo "Waiting for container to start..."
                    sleep 10
                    
                    if docker ps | grep -q ${CONTAINER_NAME}; then
                        echo "Container is running"
                        docker ps | grep ${CONTAINER_NAME}
                    else
                        echo "ERROR: Container is not running"
                        docker ps -a | grep ${CONTAINER_NAME}
                        docker logs ${CONTAINER_NAME} --tail 50
                        exit 1
                    fi
                    
                    echo "Testing API endpoint..."
                    curl -f https://api.kickify.site/health || echo "Warning: Health endpoint not available"
                    
                    echo "Deployment verified successfully"
                '''
            }
        }
        
        stage('Cleanup Old Images') {
            steps {
                echo 'Cleaning up old Docker images...'
                sh '''
                    docker images ${DOCKER_IMAGE} --format "{{.Tag}}" | \
                    grep -v latest | \
                    tail -n +4 | \
                    xargs -I {} docker rmi ${DOCKER_IMAGE}:{} 2>/dev/null || true
                    
                    docker image prune -f
                    echo "Cleanup completed"
                '''
            }
        }
    }
    
    post {
        success {
            echo 'DEPLOYMENT SUCCESSFUL'
            sh '''
                echo "Build number: ${BUILD_NUMBER}"
                echo "Container status:"
                docker ps | grep ${CONTAINER_NAME}
            '''
        }
        
        failure {
            echo 'DEPLOYMENT FAILED'
            sh '''
                echo "Checking container logs..."
                docker logs ${CONTAINER_NAME} --tail 100 || true
                
                echo "Container status:"
                docker ps -a | grep ${CONTAINER_NAME} || true
            '''
        }
        
        always {
            echo 'Cleaning up workspace...'
            cleanWs()
        }
    }
}