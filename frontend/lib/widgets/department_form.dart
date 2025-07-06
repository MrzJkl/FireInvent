import 'package:flameguardlaundry/models/create_models/create_department_model.dart';
import 'package:flameguardlaundry/models/department_model.dart';
import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';

class DepartmentForm extends StatefulWidget {
  final DepartmentModel? initial;
  final void Function(CreateDepartmentModel) onSubmit;

  const DepartmentForm({super.key, this.initial, required this.onSubmit});

  @override
  State<DepartmentForm> createState() => _DepartmentFormState();
}

class _DepartmentFormState extends State<DepartmentForm> {
  final _formKey = GlobalKey<FormBuilderState>();

  @override
  Widget build(BuildContext context) {
    return FormBuilder(
      key: _formKey,
      initialValue: {
        'name': widget.initial?.name ?? '',
        'description': widget.initial?.description ?? '',
      },
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          FormBuilderTextField(
            name: 'name',
            decoration: const InputDecoration(labelText: 'Name'),
            validator: FormBuilderValidators.compose([
              FormBuilderValidators.required(),
            ]),
          ),
          FormBuilderTextField(
            name: 'description',
            decoration: const InputDecoration(labelText: 'Beschreibung'),
            maxLines: 2,
          ),
          const SizedBox(height: 16),
          Row(
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('Abbrechen'),
              ),
              ElevatedButton(
                onPressed: () {
                  if (_formKey.currentState?.saveAndValidate() ?? false) {
                    final values = _formKey.currentState!.value;
                    widget.onSubmit(
                      CreateDepartmentModel(
                        name: values['name'] as String,
                        description:
                            (values['description'] as String).isEmpty
                                ? null
                                : values['description'] as String,
                      ),
                    );
                  }
                },
                child: const Text('Speichern'),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
