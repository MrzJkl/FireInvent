// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'person_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

PersonModel _$PersonModelFromJson(Map<String, dynamic> json) => PersonModel(
  id: json['id'] as String?,
  firstName: json['firstName'] as String,
  lastName: json['lastName'] as String,
  remarks: json['remarks'] as String?,
  contactInfo: json['contactInfo'] as String?,
  externalId: json['externalId'] as String?,
);

Map<String, dynamic> _$PersonModelToJson(PersonModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'firstName': instance.firstName,
      'lastName': instance.lastName,
      'remarks': instance.remarks,
      'contactInfo': instance.contactInfo,
      'externalId': instance.externalId,
    };
