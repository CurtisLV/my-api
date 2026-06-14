#!/bin/bash
set -e

if [ -z "$1" ] || [ -z "$2" ]; then
  echo "Error: Missing arguments."
  echo "Usage: $0 <github_username/repo> <docker_username/repo>"
  exit 1
fi

GITHUB_REPO=$1
DOCKER_REPO=$2
TEMP_DIR="temp_clone_dir"

# 1. Clone
echo "Cloning https://github.com/${GITHUB_REPO}..."
git clone "https://github.com/${GITHUB_REPO}.git" "$TEMP_DIR"

# 2. Build
echo "Building Docker image: ${DOCKER_REPO}..."
cd "$TEMP_DIR"
docker build -t "$DOCKER_REPO" .

# 3. Login
echo "Logging in to Docker Hub..."
echo "$DOCKER_PWD" | docker login -u "$DOCKER_USER" --password-stdin

# 4. Push
echo "Pushing image ${DOCKER_REPO} to Docker Hub..."
docker push "$DOCKER_REPO"

echo "Process completed successfully!"