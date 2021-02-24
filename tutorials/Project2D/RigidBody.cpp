#include "RigidBody.h"
#include "glm/ext.hpp"

Rigidbody::Rigidbody(ShapeType shapeID, glm::vec2 position, glm::vec2 velocity, float orientation, float mass, glm::vec4 colour) : PhysicsObject(shapeID, colour)
{
	this->position = position;
	this->velocity = velocity;
	this->orientation = orientation;
	this->mass = mass;
	angularVelocity = 0;
	momentOfInertia = 0;
}

Rigidbody::~Rigidbody()
{
}

void Rigidbody::fixedUpdate(glm::vec2 gravity, float timeStep)
{
	position += velocity * timeStep;
	applyForce(gravity * mass, position);
	orientation += angularVelocity * timeStep;
}

void Rigidbody::applyForce(glm::vec2 force, glm::vec2 contactPos)
{
	// use getMass() and getMoment() here in case we ever get it to do something more than just return mass...
	velocity += force / getMass();
	angularVelocity += (force.y * contactPos.x - force.x * contactPos.y) / getMomentOfInertia();
}

void Rigidbody::resolveCollision(Rigidbody* actor2, glm::vec2 contact, glm::vec2* collisionNormal)
{
	// find the vector between their centres, or use the provided direction
	// of force, and make sure it's normalised
	glm::vec2 normal = glm::normalize(collisionNormal ? *collisionNormal : actor2->position - position);
	// get the vector perpendicular to the collision normal
	glm::vec2 perp(normal.y, -normal.x);
	// determine the total velocity of the contact points for the two objects,
	// for both linear and rotational
	// 'r' is the radius from axis to application of force
	float r1 = glm::dot(contact - position, -perp);
	float r2 = glm::dot(contact - actor2->position, perp);
	// velocity of the contact point on this object
	float v1 = glm::dot(velocity, normal) - r1 * angularVelocity;
	// velocity of contact point on actor2
	float v2 = glm::dot(actor2->velocity, normal) + r2 * actor2->angularVelocity;
	if (v1 > v2) // they're moving closer
	{
		// calculate the effective mass at contact point for each object
		// ie how much the contact point will move due to the force applied.
		float mass1 = 1.0f / (1.0f / mass + (r1 * r1) / momentOfInertia);
		float mass2 = 1.0f / (1.0f / actor2->mass + (r2 * r2) / actor2->momentOfInertia);
		float elasticity = 1;
		glm::vec2 force = (1.0f + elasticity) * mass1 * mass2 / (mass1 + mass2) * (v1 - v2) * normal;
		//apply equal and opposite forces
		applyForce(-force, contact - position);
		actor2->applyForce(force, contact - actor2->position);
	}

}